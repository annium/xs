import { reducerFactory } from '@annium/store'
import { Location } from 'history'

import { context } from '../context'

export type Startup = {
  location: Location
}

const initialState: Startup = {
  location: {
    pathname: '/',
    search: '',
    state: '',
    hash: '',
    key: '',
  },
}

export const { actions: startupActions, reducer: startupReducer } = reducerFactory(context, initialState)
  .action('setLocation', (store, location: Startup['location']) => ({ ...store, location }))
  .build()
