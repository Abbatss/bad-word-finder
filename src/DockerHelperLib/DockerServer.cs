namespace DockerHelperLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Docker.DotNet;
    using Docker.DotNet.Models;

    public class DockerServer
    {
        private const string DynamoContainerName = "dynamodb";
        private const string DynamoImageName = "amazon/dynamodb-local";
        private const string DynamoImageTag = "latest";
        private string _endpoint;

        public DockerServer() : this(/*localhost*/GetLocalhostEndpoint())
        {
        }

        public DockerServer(string endpoint)
        {
            _endpoint = endpoint;
        }

        private static string GetLocalhostEndpoint()
        {
            var res = "unix:///var/run/docker.sock";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                res = "npipe://./pipe/docker_engine";
            }

            return res;
        }

        public async Task RemoveContainersByName(string containerName)
        {
            using (var conf = new DockerClientConfiguration(new Uri(_endpoint))) // localhost
            using (var client = conf.CreateClient())
            {
                var containers = await client.Containers.ListContainersAsync(new ContainersListParameters() { All = true }).ConfigureAwait(false);
                foreach (var item in containers.Where(c => c.Names.Contains("/" + containerName)))
                {
                    await client.Containers.StopContainerAsync(item.ID, new ContainerStopParameters()).ConfigureAwait(false);
                    await client.Containers.RemoveContainerAsync(item.ID, new ContainerRemoveParameters { Force = true }).ConfigureAwait(false);
                }
            }
        }

        public async Task<ContainerListResponse> RunContainter(string containerName = DynamoContainerName, string imageName = DynamoImageName, string imageTag = DynamoImageTag)
        {
            return await RunContainter(
                new HostConfig()
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>() { { "8000/tcp", new List<PortBinding> { new PortBinding { HostIP = "localhost", HostPort = "8000" } } } },
                },
                containerName,
                imageName,
                imageTag).ConfigureAwait(false);
        }

        public async Task<ContainerListResponse> RunContainter(HostConfig hostConfig, string containerName = DynamoContainerName, string imageName = DynamoImageName, string imageTag = DynamoImageTag)
        {
            bool isNewContainer = false;
            ContainerListResponse container = null;
            using (var conf = new DockerClientConfiguration(new Uri(_endpoint))) // localhost
            using (var client = conf.CreateClient())
            {
                container = await GetContainerByName(client, containerName, imageName).ConfigureAwait(false);

                if (container == null)
                {
                    await client.Images.CreateImageAsync(new ImagesCreateParameters() { FromImage = imageName, Tag = imageTag }, new AuthConfig(), new Progress<JSONMessage>()).ConfigureAwait(false);
                    var config = new Config()
                    {
                        Hostname = "localhost",
                    };
                    container = await GetContainerByName(client, containerName, imageName).ConfigureAwait(false);
                    if (container == null)
                    {
                        var response = await client.Containers.CreateContainerAsync(new CreateContainerParameters(config)
                        {
                            Image = imageName + ":" + imageTag,
                            Name = containerName,
                            Tty = false,
                            HostConfig = hostConfig,
                        }).ConfigureAwait(false);

                        var containers = await client.Containers.ListContainersAsync(new ContainersListParameters() { All = true }).ConfigureAwait(false);
                        container = containers.First(c => c.ID == response.ID);
                        isNewContainer = true;
                    }
                }

                if (container.State != "running")
                {
                    var started = await client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters()).ConfigureAwait(false);
                    isNewContainer = true;
                    if (!started)
                    {
                        throw new Exception("Cannot start the docker container");
                    }
                }
            }

            return isNewContainer ? container : null;
        }

        private async Task<ContainerListResponse> GetContainerByName(DockerClient client, string containerName, string imageName)
        {
            var containers = await client.Containers.ListContainersAsync(new ContainersListParameters() { All = true }).ConfigureAwait(false);
            var container = containers.FirstOrDefault(c => (c.Names.Any(_ => _.Contains(containerName)) || c.Image.Contains(imageName)) && c.State == "running");
            if (container == null)
            {
                container = containers.FirstOrDefault(c => c.Names.Any(_ => _.Contains(containerName)));
            }

            return container;
        }

        public async Task StopContainer(ContainerListResponse container)
        {
            if (container != null)
            {
                using (var conf = new DockerClientConfiguration(new Uri(_endpoint))) // localhost
                using (var client = conf.CreateClient())
                {
                    await client.Containers.StopContainerAsync(container.ID, new ContainerStopParameters()).ConfigureAwait(false);
                    await client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters { Force = true }).ConfigureAwait(false);
                }
            }
        }
    }
}
