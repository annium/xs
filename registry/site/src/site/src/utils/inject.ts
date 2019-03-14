import { useObserver } from 'mobx-react-lite'
import { Context, FunctionComponent, useContext } from 'react'


export function createInject<Store>(context: Context<Store>) {
  return <OwnProps, SelectorOutput>(
    selector: (store: Store, ownProps: OwnProps) => SelectorOutput,
    Component: FunctionComponent<OwnProps & SelectorOutput>,
  ) => {
    const HOC: FunctionComponent<OwnProps> = ownProps =>
      useObserver(() => Component({ ...ownProps, ...selector(useContext(context), ownProps) }))

    HOC.displayName = Component.displayName || Component.name || 'Untitled'

    return HOC
  }
}
