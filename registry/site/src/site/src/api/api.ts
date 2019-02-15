import * as lib from 'site.lib'

export default lib.api.factory({
  url: {
    protocol: process.env.REACT_APP_API_PROTOCOL || location.protocol,
    host: process.env.REACT_APP_API_HOST || location.hostname,
    port: parseInt(process.env.REACT_APP_API_PORT || location.port, 10),
  },
})