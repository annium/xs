import { action, computed, observable, runInAction } from 'mobx'

import * as user from '../api/user'
import { User } from '../models/view/User'


export class UserStore {
  @observable public data?: User
  @observable public accessError?: string

  @computed public get isLoaded(): boolean {
    return this.data !== undefined || this.accessError !== undefined
  }

  @computed public get hasAccess(): boolean {
    return this.data !== undefined && this.accessError === undefined
  }

  @action.bound public async login(name: string, password: string) {
    const result = await user.login(name, password)

    if (result.isFailure)
      runInAction(() => { throw this.accessError = result.error })
    else
      await this.load()
  }

  @action public async load() {
    const result = await user.load()
    runInAction(() => {
      console.warn('user loaded', result)
      this.data = result.data
      this.accessError = result.error
    })
  }

  @action.bound public async logout() {
    await user.logout()
    await this.load()
  }

  @action.bound public async update(name: string, password: string) {
    const result = await user.update(name, password)

    if (!result.isFailure)
      await this.load()
  }

  @action.bound public async updateToken() {
    const result = await user.updateToken()

    if (!result.isFailure)
      await this.load()
  }
}
