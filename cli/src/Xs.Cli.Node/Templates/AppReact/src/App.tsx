import React, { ReactNode, useEffect } from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { useStore } from './stores'


type Props = RouteComponentProps & { children?: ReactNode }

export const App = ({ location, children }: Props) => {
  const startup = useStore().startup

  useEffect(() => startup.setLocation(location), [startup, location])

  return <>{children}</>
}
