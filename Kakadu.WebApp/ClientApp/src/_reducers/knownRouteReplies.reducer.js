import { knownRouteConstants } from '../_constants';

export function knownRouteReplies(state = [], action) {
    switch (action.type) {
        case knownRouteConstants.SELECT_ROUTE:
            return action.payload.replies;
        default:
            return state;
    }
}