import { Location } from 'history'
import { observable } from 'mobx'

export class StartupStore {
  @observable location: Location

  constructor() {
    this.location = {
      pathname: '/',
      search: '',
      hash: '',
      key: '',
      state: ''
    }
  }
}