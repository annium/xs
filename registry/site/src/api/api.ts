import { httpClientFactory } from '@annium/client-http'

export const api = httpClientFactory({
  url: new URL(process.env.REACT_APP_API || window.location.toString()),
})
