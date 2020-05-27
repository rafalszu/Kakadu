import React from "react";
import { connect } from "react-redux";

class Replies extends React.Component {
    render() {
        const { knownRouteReplies } = this.props;
        return (
            <div className="col-md-4">
                <ul className="list-group mb-3">
                    {knownRouteReplies && knownRouteReplies.map((reply, index) => 
                        // <Item key={route.id} route={route} onChange={this.props.handleOnChange} />
                        <p key={reply.id}>{reply.id}</p>
                    )}
                </ul>
            </div>
        )
    }
}

function mapStateToProps(state) {
    const { knownRouteReplies } = state;
    return {
        knownRouteReplies
    }
}

const connectedKnownRouteRepliesPage = connect(mapStateToProps)(Replies);
export { connectedKnownRouteRepliesPage as Replies };
