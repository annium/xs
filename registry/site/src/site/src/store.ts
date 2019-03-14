import { observable } from 'mobx'
import React from 'react'

import { StartupStore } from './data/startup'
import { UserStore } from './data/user'
import { createInject } from './utils/inject'

export type Store = {
  startup: StartupStore
  user: UserStore
}

export const store = observable({
  startup: new StartupStore(),
  user: new UserStore(),
})

export const inject = createInject(React.createContext(store))
console.warn(inject)
