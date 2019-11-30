import { Location } from 'history'
import { action, observable } from 'mobx'

export class Startup {
  @observable
  public location: Location = {
    pathname: '/',
    search: '',
    state: '',
    hash: '',
  }

  @action.bound
  public setLocation(location: Location) {
    this.location = location
  }
}
