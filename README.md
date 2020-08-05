# Dackup

Dackup is a fullstack backup tool was written by .NET Core, that is fast, efficient and secure. It supports the three major operating systems (Linux, macOS, Windows)

[![dackup](http://dockeri.co/image/huobazi/dackup)](https://hub.docker.com/r/huobazi/dackup)

## Features

- [x] Cross-Platform (win/osx/linux).
- [x] Docker Container support
- [x] No Dependence.
- [x] Archive folder or files to tar.gz
- [x] Multiple Databases source support.
- [x] Multiple Storage type support.
- [x] Multiple Notifier type support.

### Databases

- [x] [PostgreSQL](https://www.postgresql.org)
- [x] [MySQL](https://www.mysql.com)
- [x] [MongoDB](https://www.mongodb.com)
- [x] [SQL Server](https://www.microsoft.com/sql-server)
- [x] [Redis](https://redis.io)

### Archive

Archive files or folder into a `.tar.gz` file.

### Storages

- [x] Local
- [x] [Amazon S3](https://aws.amazon.com/s3)
- [x] [Alibaba Cloud Object Storage Service (OSS)](https://www.alibabacloud.com/product/oss)
- [x] FTP 

### Notifiers

- [x] Email
- [x] HttpPost
- [x] [Slack](https://slack.com)
- [x] [DingTalk](https://www.dingtalk.com)

## Usage

### Help

```bash
$ /your_path/dackup

A backup app for your server or database or desktop

Usage: dackup [options] [command]

Options:
  -?|-h|--help  Show help information

Commands:
  new           Generate a config file
  perform       Performing your backup by config

Run 'dackup [command] --help' for more information about a command.

Use arrow keys to scroll. Press 'q' to exit.
```

### Generate config

```bash
$ /your_path/dackup new

Generate a config file

Usage: dackup new [arguments] [options]

Arguments:
  model         Name of the model

Options:
  -?|-h|--help  Show help information

Use arrow keys to scroll. Press 'q' to exit.
```

### Perform

```bash
$ /your_path/dackup perform

Usage: dackup perform [options]

Options:
  --config-file <FILE>  Required. The File name of the config.
  --log-path <PATH>     op. The File path of the log.
  --tmp-path <PATH>     op. The tmp path.
  -?|-h|--help          Show help information

Use arrow keys to scroll. Press 'q' to exit.

```

## Configuration

Use the subcommand ``` new ``` to generate a task config file, such as [mockup config file](https://github.com/huobazi/dackup/blob/master/src/perform-config-mockup.config)

```bash
$ /your_path/dackup new my_first_task
```

## Docker
Compiled Docker image can be pulled from: [Docker Hub](https://hub.docker.com/r/huobazi/dackup).

```
$ docker pull huobazi/dackup:latest
$ docker run --name dackup --net=host -v /config/dackup.config:/config/dackup.config huobazi/dackup perform --config-file /config/dackup.config
```

## Crontab

```bash

$ crontab -l
0 1 * * * /your_path/dackup perform --config-file /your_path/your_first_task.config --tmp-path /your_tmp_path/first --log-path /your_log_path
0 2 * * * /your_path/dackup perform --config-file /your_path/your_second_task.config --tmp-path /your_tmp_path/second --log-path /your_log_path
0 3 * * * docker run --name dackup --net=host -v /config/dackup.config:/config/dackup.config huobazi/dackup perform --config-file /config/dackup.config

```

## Install

You can download binary from [releases](https://github.com/huobazi/dackup/releases) page and place it in $PATH directory.

Or pull it from [Docker Hub](https://hub.docker.com/r/huobazi/dackup)

## Build

Install dotnet core sdk 3.1 then run

```bash
./release.sh
```
see also: [Release Script](https://github.com/huobazi/dackup/blob/master/release.sh)

## Contributing

1. Fork it
2. Create your feature branch (`git checkout -b my-new-feature`)
3. Commit your changes (`git commit -am 'Added some feature'`)
4. Push to the branch (`git push origin my-new-feature`)
5. Create new Pull Request

## Contributors

[Contributors List](https://github.com/huobazi/dackup/graphs/contributors).

## License

MIT
