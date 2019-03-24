import { observable } from 'mobx'
import React from 'react'

import { AuthStore } from './data/auth'
import { StartupStore } from './data/startup'
import { createInject } from './utils/inject'

export type Store = {
  auth: AuthStore
  startup: StartupStore
}

export const store = observable({
  auth: new AuthStore(),
  startup: new StartupStore(),
})

export const inject = createInject(React.createContext(store))
