import { createInject } from '@annium/utils'
import { observable } from 'mobx'
import React from 'react'

import { StartupStore } from './data/startup'

export type Store = {
  startup: StartupStore
}

export const store = observable({
  startup: new StartupStore(),
})

export const inject = createInject(React.createContext(store))
