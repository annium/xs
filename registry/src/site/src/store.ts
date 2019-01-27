import { observable } from 'mobx'

import { UserStore } from './data/user'

export interface Store {
  user: UserStore
}

export default function createStore(): Store {
  return observable({
    user: new UserStore()
  })
}