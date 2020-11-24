# FaaS Runner for ASPNET Functions

FaaS Runner (or just `faas-run`) is one of the components of ASPNET Functions for OpenFaaS. It uses a plugin architecture to dynamically load a function assembly and host the function execution. It is used by the [OpenFaaS ASPNET Template](https://github.com/redpandaltd/faas-aspnet-template), but it can also be used separately.

## Installing

Go to the [Releases](https://github.com/redpandaltd/faas-run/releases) page and download the appropriate binary for your platform. You can also pull the Docker image from [Docker Hub](https://hub.docker.com/r/redpandaltd/faas-run).

## Usage

The purpose of `faas-run` is to load and serve a function assembly. Assuming you are in the project folder, you can simply pass the assembly path as an argument.

```shell
~/source/hello$ faas-run bin/Debug/netstandard2.0/function.dll
OpenFaaS ASPNET Function Loader

To debug attach to process id 1212.

Loaded function assembly 'bin/Debug/netstandard2.0/function.dll'.

Running...
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://[::]:9000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

By default, `faas-run` listens on port 9000. You can change this using the `--port` option.

### Configuration

If a `config.json` file exists, it is added to the configuration pipeline, thus, available for the function in runtime. You can also specify a different configuration file by using the `--config` option.
