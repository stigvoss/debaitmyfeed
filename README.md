# DebaitMyFeed

DebaitMyFeed improves your RSS feed experience by acting as a proxy between your RSS feed reader and the source feeds
you wish to consume.

Upon passing through the proxy, the articles in the feed will be scraped and sent to an LLM to create a headline focused
on being transparent and free of clickbait.

## Supported RSS feeds

Due to the nature of needing to scrape article content from sources, support for a source need to be added manually by inheriting from the abstract `FeedDebaiter` class.

Currently, the LLM prompts are shaped to support Danish feed sources.

The following sources are supported.

### [dr.dk](https://dr.dk/)

| Feed Name                       | URL                 |
|---------------------------------|---------------------|
| Alle nyheder                    | /dr.dk/allenyheder  |
| Seneste nyt                     | /dr.dk/senestenyt   |
| Indland                         | /dr.dk/indland      |
| Udland                          | /dr.dk/udland       |
| Penge                           | /dr.dk/penge        |
| Politik                         | /dr.dk/politik      |
| Sporten                         | /dr.dk/sporten      |
| Seneste sport                   | /dr.dk/senestesport |
| Viden                           | /dr.dk/viden        |
| Kultur                          | /dr.dk/kultur       |
| Musik                           | /dr.dk/musik        |
| Vejret                          | /dr.dk/vejret       |
| Regionale                       | /dr.dk/regionale    |
| Regional: Hovedstadsområdet     | /dr.dk/kbh          |
| Regional: Bornholm              | /dr.dk/bornholm     |
| Regional: Syd- og Sønderjylland | /dr.dk/syd          |
| Regional: Fyn                   | /dr.dk/fyn          |
| Regional: Midt- og Vestjylland  | /dr.dk/vest         |
| Regional: Nordjylland           | /dr.dk/nord         |
| Regional: Trekantområdet        | /dr.dk/trekanten    |
| Regional: Sjælland              | /dr.dk/sjaelland    |
| Regional: Østjylland            | /dr.dk/oestjylland  |

### [jv.dk](https://jv.dk)

| Feed Name   | URL                 |
|-------------|---------------------|
| Forside     | /jv.dk/forside      |
| Danmark     | /jv.dk/danmark      |
| Erhverv     | /jv.dk/erhverv      |
| Sport       | /jv.dk/sport        |
| Esbjerg FB  | /jv.dk/esbjerg-fb   |
| SønderjyskE | /jv.dk/soenderjyske |
| Kolding IF  | /jv.dk/kolding-if   |
| Aabenraa    | /jv.dk/aabenraa     |
| Billund     | /jv.dk/billund      |
| Esbjerg     | /jv.dk/esbjerg      |
| Responsys   | /jv.dk/responsys    |
| Haderslev   | /jv.dk/haderslev    |
| Kolding     | /jv.dk/kolding      |
| Sønderborg  | /jv.dk/soenderborg  |
| Tønder      | /jv.dk/toender      |
| Varde       | /jv.dk/varde        |
| Vejen       | /jv.dk/vejen        |

### [sonderborgnyt.dk](https://sonderborgnyt.dk)

| Feed Name | URL                    |
|-----------|------------------------|
| Nyheder   | /sonderborgnyt.dk/feed |

## Supported LLMs

- Mistral AI through [Mistral.SDK](https://github.com/tghamm/Mistral.SDK).
- OpenAI through [OpenAI](https://github.com/openai/openai-dotnet)
- Ollama through [OllamaSharp](https://github.com/awaescher/OllamaSharp)

At least one LLM provider (called a "Headline Strategy") has to be configured.

Any provider left out of the configuration will not be registered and therefore unavailable for use.

A default strategy may be configured, if no default is configured and no provider is explicitly requested when calling a feed endpoint, the first registered provider will be used.

### Caching headlines

To save resources (in case of Ollama) or money (in case of a paid provider), headlines will be cached once per strategy used in memory and, if configured, in Redis. Caching in Redis helps to avoid re-requesting headlines if the service has been restarted.

### Selecting a specific provider on request

When consuming a feed from the service, it is possible to explicitly request a strategy to be used for generating headlines. Simply append the path with the following query string: `?provider=<strategy_id>`.

Supported strategy IDs are: `mistralai`, `openai` and `ollama`, but only configured strategies are available.

## Configuration

The service is configured through environment variables.

| Environment Variable          | Optional | Default Value          | Description                                                                                                                    |
|-------------------------------|----------|------------------------|--------------------------------------------------------------------------------------------------------------------------------|
| `Strategy__Default`           | Yes      | none                   | The default strategy to use, if none is given in the request query string. If not set, first registered strategy will be used. |
| `Ollama__Endpoint`            | Yes      | none                   | Endpoint for Ollama strategy provider.                                                                                         |
| `Ollama__Model`               | Yes      | none                   | Model to use for Ollama strategy provider.                                                                                     |
| `MistralAi__ApiKey`           | Yes      | none                   | API key for Mistral AI strategy provider.                                                                                      |
| `MistralAi__Model`            | Yes      | `mistral-large-latest` | Model to use for Mistral AI strategy provider.                                                                                 |
| `MistralAi__Temperature`      | Yes      | `0.5`                  | Temperature setting for Mistral AI strategy provider.                                                                          |
| `OpenAi__ApiKey`              | Yes      | none                   | API key for OpenAI strategy provider.                                                                                          |
| `OpenAi__Model`               | Yes      | `gpt-4o-mini`          | Model to use for OpenAI strategy provider.                                                                                     |
| `OpenAi__Temperature`         | Yes      | `0.5`                  | Temperature setting for OpenAI strategy provider.                                                                              |
| `Redis__Configuration`        | Yes      | none                   | Configuration for Redis caching.                                                                                               |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Yes      | none                   | Endpoint for OpenTelemetry.                                                                                                    |
| `OTEL_SERVICE_NAME`           | Yes      | none                   | Service name for OpenTelemetry.                                                                                                |

## Hosting

The easiest way to host the service is the clone this repository and spinning up the provided docker compose file by running the following command from the repository root folder:

```shell
docker compose up -d --build
```

To update the service, simply pull the latest commit and re-run the above docker compose command.

```shell
git pull
docker compose up -d --build
```

By default, the docker compose is configured to use Ollama. If you have compatible hardware, you may consider reading the [instructions here](https://hub.docker.com/r/ollama/ollama) on how to run Ollama with support for CUDA for NVIDIA GPUs or ROCM for AMD GPUs.