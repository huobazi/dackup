Dackup is a fullstack backup app, Which is similar to [backup](https://github.com/backup/backup).

## Features

- No dependencies.(see [.NET CoreRT](https://github.com/dotnet/corert) )
- Multiple Databases source support(now postgres only).
- Multiple Storage type support.
- Archive folder or files into a tar.
- Multiple Notifier type support.

## Current Support status

### Databases

- PostgreSQL

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
  gen           Generate a config file
  perform       Performing your backup by config

Run 'dackup [command] --help' for more information about a command.

Use arrow keys to scroll. Press 'q' to exit.
```

```bash
$ /your_path/dackup gen

Generate a config file

Usage: dackup gen [arguments] [options]

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

use the subcommand ``` gen ``` to generate a task config file

```bash
/your_path/dackup gen my_first_task
```


## Backup with crontab

```bash
$ crontab -l
0 0 * * * /your_path/dackup perform --config-file your_first_task.config --tmp-path /your_tmp_path/first --log-path /your_log_path
1 1 * * * /your_path/dackup perform --config-file your_second_task.config --tmp-path /your_tmp_path/second --log-path /your_log_path
```

## Build
Install dotnet core sdk 2.2 then

```
dotnet publish -r win-x64 -c release;
dotnet publish -r linux-x64 -c release;
dotnet publish -r osx-x64 -c release;
```

## License

MIT