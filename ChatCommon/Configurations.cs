﻿using Microsoft.Extensions.Configuration;

namespace ChatCommon
{
    public static class Configurations
    {
        static string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        static IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();

        public static string IpAddress = configuration["AppSettings:IpAddress"];
        public static string Port = configuration["AppSettings:Port"];
    }
}
