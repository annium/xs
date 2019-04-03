import { combinationFactory, reducerFactory } from '@annium/store'
import { AsyncState, createAsync } from '@annium/utils'
import { pick } from 'lodash'

import * as userApi from '../api/user'
import { context } from '../context'
import { User } from '../models/view/User'


export type Auth = {
  user: AsyncState<User | undefined>
  access: boolean
}

const user = createAsync<User | undefined>(context, undefined)

const raw = reducerFactory(context, false)
  .action('setAccess', (store, access: boolean) => access)
  .function('load', ({ setAccess }) => async () => {
    user.actions.start({})
    const userResult = await userApi.load()
    user.actions.complete(userResult)
    setAccess(userResult.isSuccess)
  })
  .function('login', ({ setAccess }) => async ({ name, password }: { name: string, password: string }) => {
    user.actions.start({})
    const loginResult = await userApi.login(name, password)
    if (loginResult.isFailure) {
      user.actions.complete({ ...loginResult, data: undefined })
      setAccess(false)

      return
    }

    const userResult = await userApi.load()
    user.actions.complete(userResult)
    setAccess(userResult.isSuccess)
  })
  .function('logout', ({ setAccess }) => async () => {
    user.actions.start({})
    await userApi.logout()
    user.actions.complete({ data: undefined, plainErrors: [], labeledErrors: {}, isSuccess: false, isFailure: false })
    setAccess(false)
  })
  .function('update', ({ setAccess }) => async ({ name, password }: { name: string, password: string }) => {
    user.actions.start({})
    const updateResult = await userApi.update(name, password)
    if (updateResult.isFailure) {
      user.actions.complete({ ...updateResult, data: undefined })
      setAccess(false)

      return
    }

    const userResult = await userApi.load()
    user.actions.complete(userResult)
    setAccess(userResult.isSuccess)
  })
  .function('updateToken', ({ setAccess }) => async () => {
    user.actions.start({})
    const updateResult = await userApi.updateToken()
    if (updateResult.isFailure) {
      user.actions.complete({ ...updateResult, data: undefined })
      setAccess(false)

      return
    }

    const userResult = await userApi.load()
    user.actions.complete(userResult)
    setAccess(userResult.isSuccess)
  })
  .build()

export const authActions = pick(raw.actions, ['load', 'login', 'logout', 'update', 'updateToken'])
export const authReducer = combinationFactory().add('user', user.reducer).add('access', raw.reducer).build()
