import { knownRouteConstants } from '../_constants';

export function knownRoute(state = {}, action) {
    switch (action.type) {
        case knownRouteConstants.SELECT_ROUTE:
            return action.payload;
        default:
            return state;
    }
}