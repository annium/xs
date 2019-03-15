import React, { ReactNode, useEffect } from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { Root } from './components/Root'
import { inject, Store } from './store'


type Props = Pick<Store, 'startup' | 'user'> & RouteComponentProps & { children?: ReactNode }

const log = console.log.bind(console, 'PersonalArea')

export const PersonalArea = inject<RouteComponentProps, Pick<Store, 'startup' | 'user'>>(
  ({ startup, user }) => ({ startup, user }),
  ({ startup, user, history, children }: Props) => {
    useEffect(
      () => {
        log('mount', 'ensure access')
        ensureAccess(user, history)
        startup.load()
      },
      [],
    )

    useEffect(() => {
      log('update', 'ensure access')
      ensureAccess(user, history)
    })

    if (!user.hasAccess) return null

    log('render')

    return (
      <Root>
        {children}
      </Root>
    )
  },
)

const ensureAccess = (user: Props['user'], history: Props['history']) => {
  log('checkAccess', user.hasAccess)
  if (user.isLoaded && !user.hasAccess)
    history.replace('/login')
}

