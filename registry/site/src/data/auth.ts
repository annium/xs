import { async } from '@annium/utils'
import { AsyncState } from '@annium/utils/dist/async'
import { action, computed, observable, runInAction } from 'mobx'

import * as user from '../api/user'
import { User } from '../models/view/User'


export class AuthStore {
  @observable public user: AsyncState<User | undefined> = async.create<User | undefined>(undefined)
  @computed public get isRunning(): boolean {
    return this.user.isRunning
  }
  @computed public get isLoaded(): boolean {
    return this.user.isSuccess || this.user.isFailure
  }
  @computed public get hasAccess(): boolean {
    return this.user.data !== undefined && this.user.isSuccess
  }
  @action.bound public async login(name: string, password: string) {
    const result = await user.login(name, password)

    if (result.isFailure)
      runInAction(() => { throw async.complete(this.user, result).error })
    else
      await this.load()
  }
  @action public async load() {
    async.load(this.user)
    const result = await user.load()
    runInAction(() => {
      console.warn('user loaded', result)
      async.complete(this.user, result)
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
