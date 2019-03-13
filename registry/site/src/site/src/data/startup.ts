import { Location } from 'history'
import { action, observable, runInAction } from 'mobx'

import * as registry from '../api/registry'


export class StartupStore {
  @observable public location: Location
  @observable public servers: { [key: string]: URL }

  constructor() {
    this.location = {
      pathname: '/',
      search: '',
      state: '',
      hash: '',
      key: '',
    }
    this.servers = {}
  }

  @action public async load() {
    const result = await registry.load()
    runInAction(() => { this.servers = result.data })
  }
}
