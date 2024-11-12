using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

class Program
{
    static void Main(string[] args)
    {
        // Ask the user for the directory containing XML and JSON files
        Console.Write("Please enter the path to the directory containing XML and JSON files: ");
        string configDirectory = Console.ReadLine();

        // Read all XML and JSON files from the directory
        var files = Directory.EnumerateFiles(configDirectory, "*.xml").Concat(Directory.EnumerateFiles(configDirectory, "*.json")).ToList();

        // Prepare data structure for ConfigMap
        var configMap = new ConfigMap
        {
            ApiVersion = "v1",
            Kind = "ConfigMap",
            Metadata = new Metadata { Name = "my-config-map" },
            Data = new Dictionary<string, string>()
        };

        foreach (var file in files)
        {
            string fileContent = File.ReadAllText(file);
            string fileName = Path.GetFileName(file);
            configMap.Data[fileName] = fileContent;
        }

        // Serialize the config map to YAML
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        string yaml = serializer.Serialize(configMap);

        // Save the output to configmap.yaml
        string outputPath = Path.Combine(configDirectory, "configmap.yaml");
        File.WriteAllText(outputPath, yaml);

        Console.WriteLine("ConfigMap YAML has been created at: " + outputPath);

        // Ask the user for the path where the ConfigMap should be mounted (e.g., /app/Common)
        Console.Write("Please enter the path where the ConfigMap should be mounted (e.g., /app/Common): ");
        string mountPath = Console.ReadLine();

        // Prepare data structure for CronJob
        var cronJob = new CronJob
        {
            ApiVersion = "batch/v1",
            Kind = "CronJob",
            Metadata = new Metadata { Name = "my-cronjob" },
            Spec = new CronJobSpec
            {
                Schedule = "0 0 * * *",  // Example schedule: run every day at midnight
                JobTemplate = new JobTemplate
                {
                    Spec = new JobSpec
                    {
                        Template = new PodTemplate
                        {
                            Spec = new PodSpec
                            {
                                Containers = new List<Container>
                                {
                                    new Container
                                    {
                                        Name = "my-container",
                                        Image = "my-image:latest",
                                        VolumeMounts = new List<VolumeMount>
                                        {
                                            new VolumeMount
                                            {
                                                Name = "config-volume",
                                                MountPath = mountPath
                                            }
                                        }
                                    }
                                },
                                Volumes = new List<Volume>
                                {
                                    new Volume
                                    {
                                        Name = "config-volume",
                                        ConfigMap = new ConfigMapVolumeSource
                                        {
                                            Name = "my-config-map"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Serialize the cronjob to YAML
        string cronJobYaml = serializer.Serialize(cronJob);

        // Save the output to cronjob.yaml
        string cronJobOutputPath = Path.Combine(configDirectory, "cronjob.yaml");
        File.WriteAllText(cronJobOutputPath, cronJobYaml);

        Console.WriteLine("CronJob YAML has been created at: " + cronJobOutputPath);
    }
}

public class ConfigMap
{
    public string ApiVersion { get; set; }
    public string Kind { get; set; }
    public Metadata Metadata { get; set; }
    public Dictionary<string, string> Data { get; set; }
}

public class Metadata
{
    public string Name { get; set; }
}

public class CronJob
{
    public string ApiVersion { get; set; }
    public string Kind { get; set; }
    public Metadata Metadata { get; set; }
    public CronJobSpec Spec { get; set; }
}

public class CronJobSpec
{
    public string Schedule { get; set; }
    public JobTemplate JobTemplate { get; set; }
}

public class JobTemplate
{
    public JobSpec Spec { get; set; }
}

public class JobSpec
{
    public PodTemplate Template { get; set; }
}

public class PodTemplate
{
    public PodSpec Spec { get; set; }
}

public class PodSpec
{
    public List<Container> Containers { get; set; }
    public List<Volume> Volumes { get; set; }
}

public class Container
{
    public string Name { get; set; }
    public string Image { get; set; }
    public List<VolumeMount> VolumeMounts { get; set; }
}

public class VolumeMount
{
    public string Name { get; set; }
    public string MountPath { get; set; }
}

public class Volume
{
    public string Name { get; set; }
    public ConfigMapVolumeSource ConfigMap { get; set; }
}

public class ConfigMapVolumeSource
{
    public string Name { get; set; }
}
