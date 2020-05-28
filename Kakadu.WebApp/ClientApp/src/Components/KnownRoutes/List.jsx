import React from "react";
import { connect } from "react-redux";
import { Item } from './Item';
import { Replies } from './Replies'
import { ReplyDetails } from "./ReplyDetails";

class List extends React.Component {
    render() {
        const { knownRoutes, knownRouteReply, className } = this.props;

        return (
            <React.Fragment>
                <div className={className}>
                    <div className="col-md-6">
                        <ul className="list-group mb-3">
                            {knownRoutes && knownRoutes.map((route, index) => 
                                <Item key={route.id} route={route} />
                            )}
                        </ul>
                    </div>
                    <div className="col-md-6">
                        <Replies />
                    </div>
                </div>
                {knownRouteReply && 
                    <div className={className}>
                        <ReplyDetails />
                    </div>
                }
            </React.Fragment>
        )
    }
}

function mapStateToProps(state) {
    const { services, knownRouteReply } = state;
    const { item } = services;
    let knownRoutes = item && item.knownRoutes;
    return {
        knownRoutes,
        knownRouteReply
    };
}

const connectedKnownRoutesPage = connect(mapStateToProps)(List);
export { connectedKnownRoutesPage as List };