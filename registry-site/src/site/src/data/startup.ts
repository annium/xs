import { Location } from 'history'
import { action, observable, runInAction } from 'mobx'

import registry from '../api/registry'


export class StartupStore {
  @observable location: Location
  @observable servers: { [key: string]: URL }

  constructor() {
    this.location = {
      pathname: '/',
      search: '',
      hash: '',
      key: '',
      state: ''
    }
    this.servers = {}
  }

  @action async load() {
    const result = await registry.load()
    runInAction(() => { this.servers = result.data })
  }
}