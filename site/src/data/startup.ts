import { reducerFactory } from '@annium/store'
import { Location } from 'history'

import * as registry from '../api/registry'
import { context } from '../context'

export type Startup = {
  location: Location
  servers: { [key: string]: URL }
}

const initialState: Startup = {
  location: {
    pathname: '/',
    search: '',
    state: '',
    hash: '',
    key: '',
  },
  servers: {},
}

export const { actions: startupActions, reducer: startupReducer } = reducerFactory(context, initialState)
  .action('setLocation', (store, location: Startup['location']) => ({ ...store, location }))
  .action('setServers', (store, servers: Startup['servers']) => ({ ...store, servers }))
  .function('load', ({ setServers }) => async () => {
    const result = await registry.load()
    setServers(result.data.servers)
  })
  .build()
