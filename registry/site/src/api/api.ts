import { factory } from '@annium/server-http'

export const api = factory({
  url: new URL(process.env.REACT_APP_API || window.location.toString()),
})
