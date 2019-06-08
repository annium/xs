import React, { ReactNode, useEffect } from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { Loader } from './components/Loader'
import { authActions } from './data/auth'
import { startupActions } from './data/startup'
import { connect, Store } from './store'

type OwnProps = RouteComponentProps & { children?: ReactNode }
type SelectorProps = Pick<Store['auth'], 'user'>
type Props = OwnProps & SelectorProps

const log = console.log.bind(console, 'App')
export const App = connect<OwnProps, SelectorProps>(
  ({ auth }) => ({ user: auth.user }),
  function App(props: Props) {
    const { user, location, children } = props

    useEffect(() => {
      log('effect', 'set location and load user')
      startupActions.setLocation(location)
      authActions.load({})
      // eslint-disable-next-line
    }, [])

    log('render', user.data)

    if (user.isLoading) return <Loader isLoading={user.isLoading} size="big" />

    return children as JSX.Element
  },
)
