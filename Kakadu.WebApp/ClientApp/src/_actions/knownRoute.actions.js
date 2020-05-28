import { knownRouteConstants } from '../_constants';

export const knownRouteActions = {
    selectKnownRoute,
    selectKnownRouteReply
};

function selectKnownRoute(route) {
    return dispatch => {
        dispatch(select(route));
    };

    function select(route) { 
        return { 
            type: knownRouteConstants.SELECT_ROUTE, 
            payload: route
        }
    }
}

function selectKnownRouteReply(reply) {
    return dispatch => {
        dispatch(select(reply));
    };

    function select(reply) {
        return {
            type: knownRouteConstants.SELECT_REPLY,
            payload: reply
        }
    }
}