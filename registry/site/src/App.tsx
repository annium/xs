import React, { ReactNode, useEffect } from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { Loader } from './components/Loader'
import { authActions } from './data/auth'
import { connect, Store } from './store'


type Props = Pick<Store, 'auth' | 'startup'> & RouteComponentProps & { children?: ReactNode }

const log = console.log.bind(console, 'App')
export const App = connect<Props, Store>(
  ({ auth, startup }) => ({ auth, startup }),
  function App(props: Props) {
    const { auth, startup, location, children } = props

    useEffect(
      () => {
        startup.location = location
        log('effect', 'load user')
        authActions.load({})
      },
      [],
    )

    log('render', auth.user.data)

    if (auth.user.isLoading)
      return <Loader isLoading={auth.user.isLoading} size="big" />

    return children as JSX.Element
  },
)
