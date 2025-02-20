# DebaitMyFeed

DebaitMyFeed improves your RSS feed experience by acting as a proxy between your RSS feed reader and the source feeds
you wish to consume.

Upon passing through the proxy, the articles in the feed will be scraped and sent to an LLM to create a headline focused
on being transparent and free of clickbait.

## Supported LLMs

- Mistral AI through [Mistral.SDK](https://github.com/tghamm/Mistral.SDK).
- OpenAI through [OpenAI](https://github.com/openai/openai-dotnet)
- Ollama through [OllamaSharp](https://github.com/awaescher/OllamaSharp)

## Supported RSS feeds

Currently, the LLM prompts are shaped to support Danish feed sources.

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

