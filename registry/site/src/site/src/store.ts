import { observable } from 'mobx'

import { StartupStore } from './data/startup'
import { UserStore } from './data/user'

export type Store = {
  startup: StartupStore
  user: UserStore
}

export function createStore(): Store {
  return observable({
    startup: new StartupStore(),
    user: new UserStore(),
  })
}
