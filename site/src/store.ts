import { combinationFactory, createConnect, createStore } from '@annium/store'

import { context } from './context'
import { Auth, authReducer } from './data/auth'
import { Startup, startupReducer } from './data/startup'

export type Store = {
  auth: Auth
  startup: Startup
}

const reducer = combinationFactory()
  .add('auth', authReducer)
  .add('startup', startupReducer)
  .build()

createStore<Store>(context, reducer)

export const connect = createConnect(context)
