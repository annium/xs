import React, { ReactNode, useEffect } from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { startupActions } from './data/startup'


type Props = RouteComponentProps & { children?: ReactNode }

export const App = ({ location, children }: Props) => {
  useEffect(() => startupActions.setLocation(location), [location])

  return <>{children}</>
}
