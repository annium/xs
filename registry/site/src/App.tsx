import React, { ReactNode, useEffect } from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { Loader } from './components/Loader'
import { inject, Store } from './store'


type Props = Pick<Store, 'auth' | 'startup'> & RouteComponentProps & { children?: ReactNode }

const log = console.log.bind(console, 'App')
export const App = inject<Props, Store>(
  ({ auth, startup }) => ({ auth, startup }),
  function App(props: Props) {
    const { auth, startup, location, children } = props

    useEffect(
      () => {
        startup.location = location
        log('effect', 'load user')
        auth.load()
      },
      [],
    )

    log('render', auth.user.data)

    if (auth.isRunning)
      return <Loader isLoading={auth.isRunning} size="big" />

    return children as JSX.Element
  },
)
