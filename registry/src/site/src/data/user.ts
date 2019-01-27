import { action, computed, observable, runInAction } from 'mobx'

import user from '../api/user'
import User from '../models/view/User'

export class UserStore {
  @observable data: User | null = null
  @observable error: string | null = null

  @computed get hasAccess(): boolean {
    return this.data !== null && this.error === null
  }

  @action.bound async login(name: string, password: string) {
    const result = await user.login(name, password)

    if (result.isFailure)
      runInAction(() => this.error = result.error)
    else
      await this.load()
  }

  @action async load() {
    const result = await user.load()
    runInAction(() => {
      console.warn('user loaded')
      this.data = result.data
      this.error = result.error
    })
  }

  @action.bound async logout() {
    const result = await user.logout()
    runInAction(() => this.error = result.error)

    await this.load()
  }
}