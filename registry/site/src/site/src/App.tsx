import React, { ReactNode, useEffect } from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { Loader } from './components/Loader'
import { inject, Store } from './store'


type Props = Pick<Store, 'startup' | 'user'> & RouteComponentProps & { children?: ReactNode }

const log = console.log.bind(console, 'App')
export const App = inject<Props, Store>(
  ({ startup, user }) => ({ startup, user }),
  function App(props: Props) {
    const { startup, user, location, children } = props

    useEffect(
      () => {
        startup.location = location
        log('effect', 'load user')
        user.load()
      },
      [user.isLoaded],
    )

    log('render', user.data)

    if (!user.isLoaded)
      return <Loader isLoading={!user.isLoaded} size="big" />

    return children as JSX.Element
  },
)
