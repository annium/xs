import React, { ReactNode, useEffect } from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { inject, Store } from './store'


type Props = Pick<Store, 'startup'> & RouteComponentProps & { children?: ReactNode }

export const App = inject<Props, Store>(
  ({ startup }) => ({ startup }),
  ({ startup, location, children }: Props) => {
    useEffect(() => { startup.location = location }, [])

    return <>{children}</>
  },
)
