import message from 'antd/lib/message'
import { Location } from 'history'
import React, { ReactNode, useEffect } from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { authActions } from '../../data/auth'
import { connect } from '../../store'

import { LoginForm } from './Form'
import styles from './index.module.scss'

type OwnProps = RouteComponentProps & { children?: ReactNode }
type SelectorProps = {
  isUserLoaded: boolean
  isUserLoadFailed: boolean
  userHasAccess: boolean
  location: Location
}
type Props = OwnProps & SelectorProps

const log = console.log.bind(console, 'LoginPage')

export const LoginPage = connect<OwnProps, SelectorProps>(
  ({ auth, startup }) => ({
    isUserLoaded: auth.user.isSuccess,
    isUserLoadFailed: auth.user.isFailure,
    userHasAccess: auth.access,
    location: startup.location,
  }),
  ({
    isUserLoaded,
    isUserLoadFailed,
    userHasAccess,
    location,
    history,
  }: Props) => {
    useEffect(() => {
      log('update', 'ensure access')
      ensureAccess(
        isUserLoaded,
        isUserLoadFailed,
        userHasAccess,
        location,
        history,
      )
    }, [isUserLoaded, isUserLoadFailed, userHasAccess, location, history])

    if (userHasAccess) return null

    return (
      <div className={styles.page}>
        <LoginForm onSubmit={handleLogin} />
      </div>
    )
  },
)

const handleLogin = (name: string, password: string) =>
  authActions
    .login({ name, password })
    .catch(error => message.error(`login failed: ${error}`))

const ensureAccess = (
  isUserLoaded: boolean,
  isUserLoadFailed: boolean,
  userHasAccess: boolean,
  location: Location,
  history: Props['history'],
) => {
  log('checkAccess', userHasAccess)
  if ((isUserLoaded || isUserLoadFailed) && userHasAccess)
    if (location.pathname.startsWith('/login')) history.replace('/')
    else history.replace(location)
}
