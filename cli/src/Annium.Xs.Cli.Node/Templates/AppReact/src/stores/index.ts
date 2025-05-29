import { createStore } from '@annium/utils'

import { Startup } from './startup'

type Store = {
  startup: Startup
}

export const { StoreProvider, useStore } = createStore<Store>({
  startup: new Startup(),
})
