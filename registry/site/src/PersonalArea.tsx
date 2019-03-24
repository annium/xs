import React, { ReactNode, useEffect } from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { Root } from './components/Root'
import { inject, Store } from './store'


type Props = Pick<Store, 'auth' | 'startup'> & RouteComponentProps & { children?: ReactNode }

const log = console.log.bind(console, 'PersonalArea')

export const PersonalArea = inject<RouteComponentProps, Pick<Store, 'auth' | 'startup'>>(
  ({ auth, startup }) => ({ auth, startup }),
  ({ auth, startup, history, children }: Props) => {
    useEffect(
      () => {
        log('mount', 'ensure access')
        ensureAccess(auth, history)
        startup.load()
      },
      [],
    )

    useEffect(() => {
      log('update', 'ensure access')
      ensureAccess(auth, history)
    })

    if (!auth.hasAccess) return null

    log('render')

    return (
      <Root>
        {children}
      </Root>
    )
  },
)

const ensureAccess = (user: Props['auth'], history: Props['history']) => {
  log('checkAccess', user.hasAccess)
  if (user.isLoaded && !user.hasAccess)
    history.replace('/login')
}

