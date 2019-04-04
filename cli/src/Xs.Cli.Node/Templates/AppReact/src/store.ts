import { combinationFactory, createConnect, createStore } from '@annium/store'

import { context } from './context'
import { Startup, startupReducer } from './data/startup'

export type Store = {
  startup: Startup
}

const reducer = combinationFactory()
  .add('startup', startupReducer)
  .build()

createStore<Store>(context, reducer)

export const connect = createConnect(context)
