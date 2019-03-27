import { Location } from 'history'
import { observable } from 'mobx'


export class StartupStore {
  @observable public location: Location

  constructor() {
    this.location = {
      pathname: '/',
      search: '',
      state: '',
      hash: '',
      key: '',
    }
  }
}
