import { knownRouteConstants } from '../_constants';

export function knownRouteReply(state = null, action) {
    switch (action.type) {
        case knownRouteConstants.SELECT_REPLY:
            return action.payload;
        default:
            return state;
    }
}