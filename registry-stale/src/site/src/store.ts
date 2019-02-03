import { observable } from 'mobx'

import { StartupStore } from './data/startup'
import { UserStore } from './data/user'

export interface Store {
  startup: StartupStore
  user: UserStore
}

export default function createStore(): Store {
  return observable({
    startup: new StartupStore(),
    user: new UserStore(),
  })
}