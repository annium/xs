import React, { ReactNode, useEffect } from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { Root } from './components/Root'
import { startupActions } from './data/startup'
import { connect } from './store'

type OwnProps = RouteComponentProps & { children?: ReactNode }
type SelectorProps = {
  isUserLoaded: boolean
  isUserLoadFailed: boolean
  userHasAccess: boolean
}
type Props = OwnProps & SelectorProps

const log = console.log.bind(console, 'PersonalArea')

export const PersonalArea = connect<OwnProps, SelectorProps>(
  ({ auth }) => ({
    isUserLoaded: auth.user.isSuccess,
    isUserLoadFailed: auth.user.isFailure,
    userHasAccess: auth.access,
  }),
  ({
    isUserLoaded,
    isUserLoadFailed,
    userHasAccess,
    history,
    children,
  }: Props) => {
    useEffect(() => {
      log('mount', 'ensure access')
      startupActions.load({})
    }, [])
    useEffect(() => {
      log('update', 'ensure access')
      ensureAccess(isUserLoaded, isUserLoadFailed, userHasAccess, history)
      startupActions.load({})
    }, [isUserLoaded, isUserLoadFailed, userHasAccess, history])

    if (!userHasAccess) return null

    log('render')

    return <Root>{children}</Root>
  },
)

const ensureAccess = (
  isUserLoaded: boolean,
  isUserLoadFailed: boolean,
  userHasAccess: boolean,
  history: Props['history'],
) => {
  log('checkAccess', userHasAccess)
  if ((isUserLoaded || isUserLoadFailed) && !userHasAccess)
    history.replace('/login')
}
