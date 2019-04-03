import React, { ReactNode, useEffect } from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { Root } from './components/Root'
import { startupActions } from './data/startup'
import { connect, Store } from './store'


type Props = Pick<Store, 'auth' | 'startup'> & RouteComponentProps & { children?: ReactNode }

const log = console.log.bind(console, 'PersonalArea')

export const PersonalArea = connect<RouteComponentProps, Pick<Store, 'auth' | 'startup'>>(
  ({ auth, startup }) => ({ auth, startup }),
  ({ auth, startup, history, children }: Props) => {
    useEffect(
      () => {
        log('mount', 'ensure access')
        ensureAccess(auth, history)
        startupActions.load({})
      },
      [],
    )

    useEffect(() => {
      log('update', 'ensure access')
      ensureAccess(auth, history)
    })

    if (!auth.access) return null

    log('render')

    return (
      <Root>
        {children}
      </Root>
    )
  },
)

const ensureAccess = (auth: Props['auth'], history: Props['history']) => {
  log('checkAccess', auth.access)
  if ((auth.user.isSuccess || auth.user.isFailure) && !auth.access)
    history.replace('/login')
}

