Dackup is a fullstack backup app, Which is similar to [backup](https://github.com/backup/backup).

## Features

- Multiple platform (win/osx/linux).
- No dependence.
- Archive folder or files to tar.gz
- Multiple Databases source support.
- Multiple Storage type support.
- Multiple Notifier type support.

### Databases

- PostgreSQL
- Mysql (coming soon)
- MongoDB (coming soon)
- SQL Server (coming soon)

### Archive

Archive files or folder into a `.tar.gz` file.

### Storages

- Local
- [Amazon S3](https://aws.amazon.com/s3)
- [Alibaba Cloud Object Storage Service (OSS)](https://www.alibabacloud.com/product/oss)

### Notifiers

- Email
- HttpPost
- [Slack](https://slack.com/)

## Usage

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

Use the subcommand ``` new ``` to generate a task config file, such as [mockup config file](https://github.com/huobazi/dackup/blob/master/perform-config-mockup.config)

```bash
$ /your_path/dackup new my_first_task
```

## Crontab

```bash

$ crontab -l
0 0 * * * /your_path/dackup perform --config-file /your_path/your_first_task.config --tmp-path /your_tmp_path/first --log-path /your_log_path
0 2 * * * /your_path/dackup perform --config-file /your_path/your_second_task.config --tmp-path /your_tmp_path/second --log-path /your_log_path

```

## Install

You can download binary from [release](https://github.com/huobazi/dackup/releases) page and place it in $PATH directory.

## Build

Install dotnet core sdk 2.2 then

```bash
dotnet publish -r win-x64 -c release;
dotnet publish -r osx-x64 -c release;
dotnet publish -r linux-x64 -c release;
```

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