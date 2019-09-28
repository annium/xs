import { CSSProperties } from '@material-ui/styles'

export const flex = (
    flexDirection: CSSProperties['flexDirection'],
    alignItems: CSSProperties['alignItems'] = 'stretch',
    justifyContent: CSSProperties['justifyContent'] = 'flex-start',
): CSSProperties => ({
    display: 'flex',
    flexDirection,
    alignItems,
    justifyContent,
})
