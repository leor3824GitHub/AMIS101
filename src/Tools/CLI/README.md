# FSH.CLI - FullStackHero Command Line Interface

A powerful CLI tool for creating and managing FullStackHero .NET projects.

## Installation

```bash
dotnet tool install -g FSH.CLI
```

## Usage

### Create a new project

```bash
# Interactive wizard
fsh new

# Using a preset
fsh new MyApp --preset quickstart

# Full customization (non-interactive)
fsh new MyApp --type api-blazor --arch monolith --db postgres
```

### Presets

| Preset | Description |
|--------|-------------|
| `quickstart` | API + Monolith + PostgreSQL + Docker + Sample Module |
| `production` | API + Blazor + Monolith + PostgreSQL + Aspire + Terraform + CI |
| `microservices` | API + Microservices + PostgreSQL + Docker + Aspire |
| `serverless` | API + Serverless (AWS Lambda) + PostgreSQL + Terraform |

### Options

| Option | Values | Default |
|--------|--------|---------|
| `--type` | `api`, `api-blazor` | `api` |
| `--arch` | `monolith`, `microservices`, `serverless` | `monolith` |
| `--db` | `postgres`, `sqlserver`, `sqlite` | `postgres` |
| `--docker` | `true`, `false` | `true` |
| `--aspire` | `true`, `false` | `true` |
| `--sample` | `true`, `false` | `false` |
| `--terraform` | `true`, `false` | `false` |
| `--ci` | `true`, `false` | `false` |

## Features

- Interactive wizard with rich TUI
- Multiple architecture styles (Monolith, Microservices, Serverless)
- Database provider selection
- Docker and Aspire support
- Terraform infrastructure templates
- GitHub Actions CI/CD

## License

MIT
