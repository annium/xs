import { Client } from '.'


type FactoryOptions = {
  url: URL | UrlOptions | string
  init?: RequestInit
}

type UrlOptions = {
  protocol: string,
  host: string
  port?: number
  basePath?: string
}

export function factory(options: FactoryOptions): Client {
  const url = options.url instanceof URL
    ? options.url
    : new URL(typeof options.url === 'string' ? options.url : buildUrl(options.url))

  return new Client(url, options.init || {})
}

enum DefaultPort {
  Http = 80,
  Https = 443,
}

function buildUrl({ protocol, host, port, basePath }: UrlOptions) {
  let url = `${protocol}//${host}`

  switch (protocol) {
    case 'http:':
      if (port !== DefaultPort.Http) url += `:${port}`
      break
    case 'https:':
      if (port !== DefaultPort.Https) url += `:${port}`
      break
    default:
      throw new Error(`Protocol ${protocol} is not supported`)
  }

  if (basePath) url += `/${basePath}`

  return url
}
