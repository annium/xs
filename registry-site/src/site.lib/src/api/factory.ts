import 'whatwg-fetch'

import { Client } from '.'


interface FactoryOptions {
  url: URL | UrlOptions | string
  init?: RequestInit
}

interface UrlOptions {
  protocol: string,
  host: string
  port?: number
  basePath?: string
}

export default function factory(options: FactoryOptions): Client {
  const url = options.url instanceof URL
    ? options.url
    : new URL(typeof options.url === 'string' ? options.url : buildUrl(options.url))

  return new Client(url, options.init || {})
}

function buildUrl({ protocol, host, port, basePath }: UrlOptions) {
  let url = `${protocol}//${host}`

  switch (protocol) {
    case 'http:':
      if (port !== 80) url += `:${port}`
      break
    case 'https:':
      if (port !== 443) url += `:${port}`
      break
    default:
      throw `Protocol ${protocol} is not supported`
  }

  if (basePath) url += `/${basePath}`

  return url
}
